using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Services;
using Reusable.CommandLine.Tests.Helpers;
using Reusable.TestBase.Collections;

namespace Reusable.CommandLine.Tests.Collections
{
    [TestClass]
    public class ImmutableNameSetTest
    {
        [TestMethod]
        public void Equals_DifferentNames_False()
        {
            Assert.AreNotEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("bar"));
        }

        [TestMethod]
        public void Equals_SameNames_True()
        {
            Assert.AreEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("foo"));
        }

        [TestMethod]
        public void Equals_CommonNames_True()
        {
            Assert.AreEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("bar", "FOO"));
        }

        [TestMethod]
        public void From_Command_NoAttribute_DefaultName()
        {
            Assert.AreEqual(ImmutableNameSet.Create("Test1"), ImmutableNameSetFactory.CreateCommandNameSet<Test1Command>());
        }

        [TestMethod]
        public void From_Command_AllowShortName_DefaultNameAndShortName()
        {
            var name = ImmutableNameSetFactory.CreateCommandNameSet<Test2Command>();
            Assert.AreEqual(ImmutableNameSet.Create("Test2"), name);
            Assert.AreEqual(ImmutableNameSet.Create("T"), name);
        }

        [TestMethod]
        public void From_Command_CustomNames_CustomNames()
        {
            var name = ImmutableNameSetFactory.CreateCommandNameSet<Test3Command>();
            Assert.AreEqual(ImmutableNameSet.Create("test"), name);
            Assert.AreEqual(ImmutableNameSet.Create("tst"), name);
        }

        [TestMethod]
        public void From_PropertyInfo_DefaultParameter_DefaultName()
        {
            var name = ImmutableNameSetFactory.CreateParameterNameSet(typeof(Foo).GetProperty(nameof(Foo.Bar)));
            Assert.AreEqual(ImmutableNameSet.Create("Bar"), name);
        }

        [TestMethod]
        public void From_PropertyInfo_AllowShortName_DefaultAndShortNames()
        {
            var name = ImmutableNameSetFactory.CreateParameterNameSet(typeof(Foo).GetProperty(nameof(Foo.Corge)));
            Assert.AreEqual(ImmutableNameSet.Create("Corge"), name);
            Assert.AreEqual(ImmutableNameSet.Create("C"), name);
        }

        [TestMethod]
        public void From_PropertyInfo_CustomNames_CustomNames()
        {
            var name = ImmutableNameSetFactory.CreateParameterNameSet(typeof(Foo).GetProperty(nameof(Foo.Baz)));
            Assert.AreEqual(ImmutableNameSet.Create("Qux"), name);
            Assert.AreEqual(ImmutableNameSet.Create("Q"), name);
        }

        private class Foo
        {
            [Parameter]
            public string Bar { get; set; }

            [Parameter("Qux", "Q")]
            public string Baz { get; set; }

            [Parameter(AllowShortName = true)]
            public string Corge { get; set; }
        }

        private class Test1Command : ICommand
        {
            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }

            public event EventHandler CanExecuteChanged;
        }

        [CommandNames(AllowShortName = true)]
        private class Test2Command : ICommand
        {
            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }

            public event EventHandler CanExecuteChanged;
        }

        [CommandNames("test", "tst")]
        private class Test3Command : ICommand
        {
            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }

            public event EventHandler CanExecuteChanged;
        }
    }

    [TestClass]
    public class ImmutableNameSetEqualityComparerTest : EqualityComparerTest<IImmutableSet<string>>
    {
        public ImmutableNameSetEqualityComparerTest() : base(ImmutableNameSet.Comparer)
        {
            IgnoreHashCode = true;
        }

        protected override IEnumerable<(IImmutableSet<string> Left, IImmutableSet<string> Right)> GetEqualElements()
        {
            yield return (ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("FOO"));
            yield return (ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("FOO", "bar"));
            yield return (ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("foo"));
        }

        protected override IEnumerable<(IImmutableSet<string> Left, IImmutableSet<string> Right)> GetNonEqualElements()
        {
            yield return (ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("bar"));
            yield return (ImmutableNameSet.Create("baz"), ImmutableNameSet.Create("foo"));
            yield return (null, ImmutableNameSet.Create("foo"));
            yield return (null, null);
        }
    }  
}
