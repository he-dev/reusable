using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo;
using Reusable.OmniLog;
using Xunit;
using Double = Reusable.Flexo.Double;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    using static ExpressionAssert;

    public class ExpressionTest: IClassFixture<ExpressionFixture>
    {
        private readonly ExpressionFixture _helper;

        public ExpressionTest(ExpressionFixture helper)
        {
            _helper = helper;
        }
        
        [Fact]
        public void All_returns_True_when_all_True()
        {
            Equal(true, _helper.GetExpression<All>(e => e.Values = Constant.CreateMany(true, true, true).ToList()));
        }

        [Fact]
        public void All_returns_False_when_some_False() => Equal(false, new All(Logger<All>.Null) { Values = Constant.CreateMany(true, false, true).ToList() });

        [Fact]
        public void All_returns_False_when_all_False() => Equal(false, new All(Logger<All>.Null) { Values = Constant.CreateMany(false, false, false).ToList() });

        [Fact]
        public void All_flows_all_contexts()
        {
            var actual = new All(Logger<All>.Null)
            {
                Values =
                {
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"] + 1))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"] + 1))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"] + 1)))
                }
            };

            var result = Equal(true, actual, Expression.DefaultSession.SetItem("x", 1));
            Assert.Equal(4, result.Context["x"]);
        }

        [Fact]
        public void Any_returns_True_when_some_True() => Equal(true, new Any(Logger<Any>.Null) { Values = Constant.CreateMany(false, false, true).ToList() });

        [Fact]
        public void Any_returns_False_when_all_False() => Equal(false, new Any(Logger<Any>.Null) { Values = Constant.CreateMany(false, false, false).ToList() });

        [Fact]
        public void Any_flows_True_context()
        {
            var actual = new Any(Logger<Any>.Null)
            {
                Values =
                {
                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"] + 2))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"] + 3))),
                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"] + 4)))
                }
            };

            var result = Equal(true, actual, Expression.DefaultSession.SetItem("x", 1));
            Assert.Equal(4, result.Context["x"]);
        }

        [Fact]
        public void IIf_invokes_True_when_True() => Equal("foo", new IIf(Logger<IIf>.Null)
        {
            Predicate = Constant.Create(true),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [Fact]
        public void IIf_invokes_False_when_False() => Equal("bar", new IIf(Logger<IIf>.Null)
        {
            Predicate = Constant.Create(false),
            True = Constant.Create("foo"),
            False = Constant.Create("bar")
        });

        [Fact]
        public void IIf_throws_when_no_result_specified()
        {
            Assert.Throws<InvalidOperationException>(() => new IIf(Logger<IIf>.Null) { Predicate = Constant.Create(false) }.Invoke(Expression.DefaultSession));
        }

        [Fact]
        public void IIf_returns_null_constant_when_True_not_specified() => Equal(Constant.Null, new IIf(Logger<IIf>.Null)
        {
            Predicate = Constant.True,
            False = Double.Zero
        });

        [Fact]
        public void IIf_returns_null_constant_when_False_not_specified() => Equal(Constant.Null, new IIf(Logger<IIf>.Null)
        {
            Predicate = Constant.False,
            True = Double.One,
        });

        [Fact]
        public void IIf_flows_context_to_True_when_Predicate_True()
        {
            var actual = new IIf(Logger<IIf>.Null)
            {
                Predicate = Constant.True,
                True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"] + 1))),
                False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"] + 2))),
            };

            var result = Equal(1.0, actual, Expression.DefaultSession.SetItem("x", 1));
            Assert.Equal(2, result.Context["x"]);
        }

        [Fact]
        public void IIf_does_not_flow_context_to_False_when_Predicate_False()
        {
            var actual = new IIf(Logger<IIf>.Null)
            {
                Predicate = Constant.False,
                True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"] + 1))),
                False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"] + 2))),
            };

            var result = Equal(2.0, actual, Expression.DefaultSession.SetItem("x", 1));
            Assert.Equal(3, result.Context["x"]);
        }

        [Fact]
        public void Max_returns_Max() => Equal(3.0, new Max { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        [Fact]
        public void Min_returns_Min() => Equal(1.0, new Min { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        [Fact]
        public void Sum_returns_Sum() => Equal(6.0, new Sum { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        [Fact]
        public void IsEqual_returns_True_when_Input_equal_Value() => Equal(true, new IsEqual(Logger<IsEqual>.Null)
        {
            Value = Constant.Create("foo"),
        }, Expression.DefaultSession.PushExtensionInput("foo"));

        [Fact]
        public void IsEqual_returns_False_when_Input_not_equal_Value() => Equal(false, new IsEqual(Logger<IsEqual>.Null)
        {
            Value = Constant.Create("foo"),
        }, Expression.DefaultSession.PushExtensionInput("bar"));

        [Fact]
        public void IsGreaterThan_returns_True_when_Input_GreaterThan_Value() => Equal(true, new IsGreaterThan(Logger<IsGreaterThan>.Null)
        {
            Value = Constant.Create(2.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void IsGreaterThan_returns_False_when_Input_LessThan_Value() => Equal(false, new IsGreaterThan(Logger<IsGreaterThan>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(2.0));

        [Fact]
        public void GreaterThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new IsGreaterThan(Logger<IsGreaterThan>.Null)
        {
            Value = Constant.Create(2.0),
        }, Expression.DefaultSession.PushExtensionInput(2.0));

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftGreaterThanRight() => Equal(true, new IsGreaterThanOrEqual(Logger<IsGreaterThanOrEqual>.Null)
        {
            Value = Constant.Create(2.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new IsGreaterThanOrEqual(Logger<IsGreaterThanOrEqual>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void GreaterThanOrEqual_ReturnsFalseWhenLeftLessThanRight() => Equal(false, new IsGreaterThanOrEqual(Logger<IsGreaterThanOrEqual>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(2.0));

        [Fact]
        public void LessThan_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new IsLessThan(Logger<IsLessThan>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(2.0));

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new IsLessThan(Logger<IsLessThan>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new IsLessThan(Logger<IsLessThan>.Null)
        {
            Value = Constant.Create(2.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new IsLessThanOrEqual(Logger<IsLessThanOrEqual>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(2.0));

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new IsLessThanOrEqual(Logger<IsLessThanOrEqual>.Null)
        {
            Value = Constant.Create(3.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void LessThanOrEqual_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new IsLessThanOrEqual(Logger<IsLessThanOrEqual>.Null)
        {
            Value = Constant.Create(2.0),
        }, Expression.DefaultSession.PushExtensionInput(3.0));

        [Fact]
        public void Not_returns_True_when_False() => Equal(false, new Not(Logger<Not>.Null) { Value = Constant.True });

        [Fact]
        public void Not_returns_False_when_True() => Equal(false, new Not(Logger<Not>.Null) { Value = Constant.True });

        [Fact]
        public void ToDouble_maps_True_to_One() => Equal(1.0, new ToDouble { Value = Constant.True });

        [Fact]
        public void ToDouble_maps_False_to_Zero() => Equal(0.0, new ToDouble { Value = Constant.False });

        [Fact]
        public void ToString_converts_Input_to_string() => Equal("1", new ToString(), Expression.DefaultSession.PushExtensionInput(1.0));

        [Fact]
        public void ToString_converts_Input_to_string_with_custom_format() => Equal("1.00", new ToString
        {
            Format = Constant.Create("{0:F2}")
        }, Expression.DefaultSession.PushExtensionInput(1.0));

        [Fact]
        public void Constant_flows_context()
        {
            var c = Constant.True;
            var actual = c.Invoke(Expression.DefaultSession.SetItem("x", 1));
            Assert.NotNull(actual.Context);
            Assert.True(actual.Context.ContainsKey("x"));
        }

        [Fact]
        public void Switch_uses_ObjectEqual_by_default()
        {
            var s = new Switch
            {
                Value = Double.One,
                Cases =
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
                }
            };

            Equal(Constant.True, s);
        }

        [Fact]
        public void Switch_passes_SwitchValue_to_context()
        {
            var s = new Switch
            {
                Value = Constant.FromValue("Test", "bar"),
                Cases =
                {
                    new SwitchCase
                    {
                        When = new Contains(Logger<Contains>.Null)
                        {
                            Values = Constant.CreateMany("foo", "bar").ToList(),
                            Value = new GetValue(Logger<GetValue>.Null)
                            {
                                Path = "SwitchSession.Value"
                            }
                        },
                        Body = Constant.True
                    },
                    new SwitchCase
                    {
                        When = Double.Zero,
                        Body = Constant.False
                    }
                }
            };

            Equal(Constant.True, s);
        }

        [Fact]
        public void Switch_uses_Default_when_no_match()
        {
            var s = new Switch
            {
                Value = Constant.FromValue("Test", "bar"),
                Cases =
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
                },
                Default = new Constant<double>("Actual", 2.0)
            };

            Equal(2.0, s);
        }

        [Fact]
        public void Contains_returns_True_when_contains_value()
        {
            Equal(true, new Contains(Logger<Contains>.Null)
            {
                Values = Constant.CreateMany("foo", "bar").ToList(),
                Value = Constant.FromValue("blub", "bar")
            });
        }

        [Fact]
        public void Contains_uses_custom_comparer()
        {
            Equal
            (
                true,
                new Contains(Logger<Contains>.Null)
                {
                    Values = Constant.CreateMany("foo", "BAR").ToList(),
                    Value = Constant.FromValue("Value", "bar"),
                    Comparer = "SoftString"
                }
            );
        }

        [Fact]
        public void Matches_return_True_when_Pattern_matches()
        {
            Equal(true, new Matches(Logger<Matches>.Null) { IgnoreCase = true, Value = Constant.FromValue("Value", "Hallo"), Pattern = Constant.FromValue("Pattern", "hallo") });
        }

        [Fact]
        public void GetContextItem_can_get_item_by_key()
        {
            Equal(1, new GetValue(Logger<GetValue>.Null) { Path = "SwitchSession.Value" }, Expression.DefaultSession.Set(Use<ISwitchSession>.Scope, x => x.Value, 1));
        }

        [Fact]
        public void Can_use_references()
        {
            var expressions = new IExpression[] { new Not(Logger<Not>.Null) { Name = "Nope" }, };
            var expression1 = new Constant<bool>("Yes", true) { This = new List<IExpression> { new Ref(Logger<Ref>.Null) { Path = "Nope" } } };
            var expression2 = new Constant<bool>("No", false) { This = new List<IExpression> { new Ref(Logger<Ref>.Null) { Path = "Nope" } } };

            Equal(false, expression1, Expression.DefaultSession.WithExpressions(expressions));
            Equal(true, expression2, Expression.DefaultSession.WithExpressions(expressions));
        }
    }
}