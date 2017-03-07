﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ConverterTest
    {
        protected object Convert<TConverter>(object arg, Type type) where TConverter : TypeConverter, new()
        {
            return TypeConverter.Empty.Add<TConverter>().Convert(arg, type);
        }

        protected ISpecificationContext<TResult> Convert<TConverter, TResult>(object arg, Type type) where TConverter : TypeConverter, new()
        {
            return TypeConverter.Empty.Add<TConverter>().Convert(arg, type).Verify().IsNotNull().Cast<TResult>();
        }
    }
}