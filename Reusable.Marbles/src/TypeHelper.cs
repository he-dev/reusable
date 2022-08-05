using System;

namespace Reusable.Marbles;

public static class TypeHelper
{
    public static Type FromAnonymous<T>(T typeObject) => typeof(T);
}