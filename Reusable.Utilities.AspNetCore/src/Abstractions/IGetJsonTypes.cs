using System;
using System.Collections.Generic;

namespace Reusable.Utilities.AspNetCore.Abstractions
{
    public interface IGetJsonTypes
    {
        IEnumerable<Type> Execute();
    }
}