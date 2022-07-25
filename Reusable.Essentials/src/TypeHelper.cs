using System;

namespace Reusable.Essentials;

public static class TypeHelper
{
    public static Type FromAnonymous<T>(T typeObject) => typeof(T);
}