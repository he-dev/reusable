using System;
using System.Collections.Generic;

namespace Reusable.Utilities.JsonNet.Abstractions;

public interface IGetJsonTypes
{
    IEnumerable<Type> Execute();
}