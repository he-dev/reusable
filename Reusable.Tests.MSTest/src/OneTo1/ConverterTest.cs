using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;

namespace Reusable.Tests.OneTo1
{
    [TestClass]
    public class ConverterTest
    {
        protected object Convert<TConverter>(object arg, Type type) where TConverter : TypeConverter, new()
        {
            return TypeConverter.Empty.Add<TConverter>().Convert(arg, type);
        }

        //protected ISpecificationContext<TResult> Convert<TConverter, TResult>(object arg, Type type) where TConverter : TypeConverter, new()
        //{
        //    return TypeConverter.Empty.Add<TConverter>().Convert(arg, type).Verify().IsNotNull().Cast<TResult>();
        //}
    }
}
