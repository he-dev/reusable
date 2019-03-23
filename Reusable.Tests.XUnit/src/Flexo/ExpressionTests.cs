using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo;
using Xunit;
using Double = Reusable.Flexo.Double;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    using static ExpressionAssert;

    public class ExpressionTest
    {
        [Fact]
        public void All_ReturnsTrueWhenAllTrue() => Equal(true, new All { Values = Constant.CreateMany(true, true, true).ToList() });

        [Fact]
        public void All_ReturnsFalseWhenSomeFalse() => Equal(false, new All { Values = Constant.CreateMany(true, false, true).ToList() });

        [Fact]
        public void All_ReturnsFalseWhenAllFalse() => Equal(false, new All { Values = Constant.CreateMany(false, false, false).ToList() });

        [Fact]
        public void All_flows_all_contexts()
        {
            var actual = new All
            {
                Values =
                {
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"].Value + 1))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"].Value + 1))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"].Value + 1)))
                }
            };

            var result = Equal(true, actual, ExpressionContext.Empty.SetItem("x", 1));
            Assert.Equal(4, result.Context["x"].Value);
        }

        [Fact]
        public void Any_ReturnsTrueWhenSomeTrue() => Equal(true, new Any { Values = Constant.CreateMany(false, false, true).ToList() });

        [Fact]
        public void Any_ReturnsFalseWhenAllFalse() => Equal(false, new Any { Values = Constant.CreateMany(false, false, false).ToList() });

        [Fact]
        public void Any_flows_True_context()
        {
            var actual = new Any
            {
                Values =
                {
                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"].Value + 2))),
                    LambdaExpression.Predicate(context => (true, context.SetItem("x", (int)context["x"].Value + 3))),
                    LambdaExpression.Predicate(context => (false, context.SetItem("x", (int)context["x"].Value + 4)))
                }
            };

            var result = Equal(true, actual, ExpressionContext.Empty.SetItem("x", 1));
            Assert.Equal(4, result.Context["x"].Value);
        }

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
            False = Double.Zero
        });

        [Fact]
        public void IIf_returns_null_constant_when_False_not_specified() => Equal(Constant.Null, new IIf
        {
            Predicate = Constant.False,
            True = Double.One,
        });

        [Fact]
        public void IIf_flows_context_to_True_when_Predicate_True()
        {
            var actual = new IIf
            {
                Predicate = Constant.True,
                True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"].Value + 1))),
                False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"].Value + 2))),
            };

            var result = Equal(1.0, actual, ExpressionContext.Empty.SetItem("x", 1));
            Assert.Equal(2, result.Context["x"].Value);
        }

        [Fact]
        public void IIf_does_not_flow_context_to_False_when_Predicate_False()
        {
            var actual = new IIf
            {
                Predicate = Constant.False,
                True = LambdaExpression.Double(context => (1.0, context.SetItem("x", (int)context["x"].Value + 1))),
                False = LambdaExpression.Double(context => (2.0, context.SetItem("x", (int)context["x"].Value + 2))),
            };

            var result = Equal(2.0, actual, ExpressionContext.Empty.SetItem("x", 1));
            Assert.Equal(3, result.Context["x"].Value);
        }

        [Fact]
        public void Max_ReturnsMax() => Equal(3.0, new Max { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        [Fact]
        public void Min_ReturnsMin() => Equal(1.0, new Min { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        [Fact]
        public void Sum_ReturnsSum() => Equal(6.0, new Sum { Values = Constant.CreateMany(2.0, 3.0, 1.0).ToList() });

        //        [Fact]
        //        public void Equals_ReturnsTrueWhenEqual() => Equal(true, new Equals
        //        {
        //            Left = Constant.Create("foo"),
        //            Right = Constant.Create("foo"),
        //        });
        //
        //        [Fact]
        //        public void Equals_ReturnsFalseWhenNotEqual() => Equal(false, new Equals
        //        {
        //            Left = Constant.Create("foo"),
        //            Right = Constant.Create("bar"),
        //        });

        [Fact]
        public void GreaterThan_ReturnsTrueWhenLeftGreaterThanRight() => Equal(true, new IsGreaterThan
        {
            Value = Constant.Create(2.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void GreaterThan_ReturnsFalseWhenLeftLessThanRight() => Equal(false, new IsGreaterThan
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(2.0)));

        [Fact]
        public void GreaterThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new IsGreaterThan
        {
            Value = Constant.Create(2.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(2.0)));

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftGreaterThanRight() => Equal(true, new IsGreaterThanOrEqual
        {
            Value = Constant.Create(2.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void GreaterThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new IsGreaterThanOrEqual
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void GreaterThanOrEqual_ReturnsFalseWhenLeftLessThanRight() => Equal(false, new IsGreaterThanOrEqual
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(2.0)));

        [Fact]
        public void LessThan_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new IsLessThan
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(2.0)));

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftEqualsRight() => Equal(false, new IsLessThan
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void LessThan_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new IsLessThan
        {
            Value = Constant.Create(2.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftLessThanRight() => Equal(true, new IsLessThanOrEqual
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(2.0)));

        [Fact]
        public void LessThanOrEqual_ReturnsTrueWhenLeftEqualsRight() => Equal(true, new IsLessThanOrEqual
        {
            Value = Constant.Create(3.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void LessThanOrEqual_ReturnsFalseWhenLeftGreaterThanRight() => Equal(false, new IsLessThanOrEqual
        {
            Value = Constant.Create(2.0),
        }, ExpressionContext.Empty.ExtensionInput(Constant.Create(3.0)));

        [Fact]
        public void Not_ReturnsTrueWhenFalse() => Equal(false, new Not { Value = Constant.True });

        [Fact]
        public void Not_ReturnsFalseWhenTrue() => Equal(false, new Not { Value = Constant.True });

        [Fact]
        public void ToDouble_MapsTrueToOne() => Equal(1.0, new ToDouble { Value = Constant.True });

        [Fact]
        public void ToDouble_MapsFalseToZero() => Equal(0.0, new ToDouble { Value = Constant.False });

        [Fact]
        public void Constant_flows_context()
        {
            var c = Constant.True;
            var actual = c.Invoke(ExpressionContext.Empty.SetItem("x", 1));
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
                        When = new Contains
                        {
                            Values = Constant.CreateMany("foo", "bar").ToList(),
                            Value = new GetContextItem
                            {
                                Key = "Switch.Value"
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
                Default = new Double("Actual", 2.0)
            };

            Equal(2.0, s);
        }

        [Fact]
        public void Contains_returns_True_when_contains_value()
        {
            Equal(true, new Contains
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
                new Contains
                {
                    Values = Constant.CreateMany("foo", "BAR").ToList(),
                    Value = Constant.FromValue("Value", "bar"),
                    Comparer = new Reusable.Flexo.SoftStringComparer(nameof(Reusable.Flexo.SoftStringComparer))
                    //                    new StringEqual
                    //                    {
                    //                        IgnoreCase = true,
                    //                        Left = new GetContextItem
                    //                        {
                    //                            Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Value)
                    //                        },
                    //                        Right = new GetContextItem
                    //                        {
                    //                            Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Item)
                    //                        }
                    //                    }
                }
            );
        }

        [Fact]
        public void Matches_return_True_when_Pattern_matches()
        {
            Equal(true, new Matches { IgnoreCase = true, Value = Constant.FromValue("Value", "Hallo"), Pattern = Constant.FromValue("Pattern", "hallo") });
        }

        [Fact]
        public void GetContextItem_can_get_item_by_key()
        {
            Equal(1, new GetContextItem { Key = "Switch.Value" }, ExpressionContext.Empty.Set(Item.For<ISwitchContext>(), x => x.Value, 1));
        }
    }
}