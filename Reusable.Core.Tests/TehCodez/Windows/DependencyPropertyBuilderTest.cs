using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Windows;

namespace Reusable.Tests.Windows
{
    [TestClass]
    public class DependencyPropertyBuilderTest
    {
        [TestMethod]
        public void Count_DefaultValue()
        {
            var testObject = new TestObject();
            Assert.AreEqual(5, testObject.Count, "Default value.");
        }

        [TestMethod]
        public void Count_ChangeValue()
        {
            var testObject = new TestObject
            {
                Count = 8
            };
            Assert.AreEqual(8, testObject.Count, "Changed value");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Count_ValueOutOfRange()
        {
            new TestObject
            {
                Count = 22
            };
        }
    }

    internal class TestObject : DependencyObject
    {
        public static readonly DependencyProperty CountProperty =
            DependencyPropertyBuilder
            .Register<TestObject, int>(nameof(Count))
            // requires #pragma warning disable 1720 
            //.Register<TestObject, int>(() => default(TestObject).Count)
            // wrapps default(T)
            //.Register<TestObject, int>(() => Default.Of<TestObject>().Count)
            .PropertyMetadata(b => b
                .PropertyChanged((testObject, e) =>
                {
                    Console.WriteLine($"{e.Property.Name} = {e.OldValue} --> {e.NewValue}");
                })            
                .CoerceValue((testObject, e) =>
                {
                    if (e.NewValue > 20)
                    {
                        e.CoercedValue = 15;
                    }

                    if (e.NewValue < 1)
                    {
                        e.Canceled = true;
                    }
                })
            );

        [DefaultValue(5)]
        [Range(0, 15)]
        public int Count
        {
            get { return CountProperty.GetValue<int>(this); }
            set { CountProperty.SetValue(this, value); }
        }
    }
}
