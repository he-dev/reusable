using System;
using JetBrains.Annotations;

namespace Reusable.Sequences
{
    public static class FibonacciSequence
    {
        [NotNull]
        public static ISequence<T> Create<T>(T one) => new FibonacciSequence<T>(one);
    }

    public static class GeometricSequence
    {
        [NotNull]
        public static ISequence<T> Create<T>(T first, T ratio) => new GeometricSequence<T>(first, ratio);
    }

    public static class ArithmeticSequence
    {
        [NotNull]
        public static ISequence<T> Create<T>(T first, T step) => new ArithmeticSequence<T>(first, step);
    }

    public static class RegularSequence
    {
        [NotNull]
        public static ISequence<T> Create<T>(T value) => new RegularSequence<T>(value);
    }

    public static class ReciprocalSequence
    {
        public static ISequence<T> Create<T>(T dividend, ISequence<T> divisors) => new ReciprocalSequence<T>(dividend, divisors);
    }

    public static class HarmonicSequence
    {
        public static ISequence<T> Create<T>(T dividend, T divisorStart, T divisorStep) => new ReciprocalSequence<T>(dividend, ArithmeticSequence.Create(divisorStart, divisorStep));
    }
}
