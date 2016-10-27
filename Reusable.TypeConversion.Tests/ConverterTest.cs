using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ConverterTest
    {
        protected object Convert<TConverter>(object arg, Type type) where TConverter : TypeConverter, new()
        {
            return TypeConverter.Empty.Add<TConverter>().Convert(arg, type);
        }

        protected IValidationContext<TResult> Convert<TConverter, TResult>(object arg, Type type) where TConverter : TypeConverter, new()
        {
            return TypeConverter.Empty.Add<TConverter>().Convert(arg, type).Verify().IsNotNull().Cast<TResult>();
        }
    }
}
