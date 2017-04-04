using System;
using System.Collections.Generic;

namespace Reusable
{
    public class Result
    {
        protected Result(Exception exception, string message, TimeSpan elapsed)
        {
            Exception = exception;
            Message = message;
            Elapsed = elapsed;
        }

        public Exception Exception { get; }
        public string Message { get; }
        public TimeSpan Elapsed { get; }

        public bool Succees => string.IsNullOrEmpty(Message) && Exception == null;
        public bool Failure => !Succees;

        public override string ToString() => Succees ? Message : (Exception?.Message ?? Message);

        public static Result Ok(TimeSpan elapsed) => new Result(null, null, elapsed);
        public static Result Ok() => Ok(TimeSpan.Zero);

        public static Result Fail(Exception exception, string message, TimeSpan elapsed) => new Result(exception, message, elapsed);

        public static Result Fail(Exception exception, string message) => Fail(exception, message, TimeSpan.Zero);
        public static Result Fail(Exception exception, TimeSpan elapsed) => Fail(exception, null, elapsed);
        public static Result Fail(Exception exception) => Fail(exception, null, TimeSpan.Zero);
        public static Result Fail(string message) => Fail(null, message, TimeSpan.Zero);

        public static implicit operator Result((Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result((Exception Exception, string Message) t) => Fail(t.Exception, t.Message);
        public static implicit operator Result((Exception Exception, TimeSpan Elapsed) t) => Fail(t.Exception, t.Elapsed);
        public static implicit operator Result(Exception exception) => Fail(exception);

        public static implicit operator bool(Result result) => result.Succees;

        public void Deconstruct(out Exception exception, out string message, out TimeSpan elapsed)
        {
            message = Message;
            exception = Exception;
            elapsed = Elapsed;
        }

        public void Deconstruct(out Exception exception, out string message)
        {
            message = Message;
            exception = Exception;
        }
    }

    public class Result<T> : Result
    {
        private readonly T _value;

        protected Result(Exception exception, string message, TimeSpan elapsed) : base(exception, message, elapsed) { }
        protected Result(T value, TimeSpan elapsed) : this(null, null, elapsed) => _value = value;

        public T Value => Succees ? _value : throw new InvalidOperationException("Value isn't available because the result is in error state.");

        public IEnumerable<TValue> AsEnumerable<TValue>() => Value as IEnumerable<TValue> ?? throw new InvalidCastException($"Cannot cast {typeof(T).Name} to {typeof(TValue).Name}.");

        public static Result<T> Ok(T value, TimeSpan elapsed) => new Result<T>(value, elapsed);
        public static Result<T> Ok(T value) => Ok(value, TimeSpan.Zero);

        public new static Result<T> Fail(Exception exception, string message, TimeSpan elapsed) => new Result<T>(exception, message, elapsed);
        public new static Result<T> Fail(Exception exception, string message) => new Result<T>(exception, message, TimeSpan.Zero);
        public new static Result<T> Fail(Exception exception, TimeSpan elapsed) => Fail(exception, null, elapsed);
        public new static Result<T> Fail(Exception exception) => Fail(exception, null, TimeSpan.Zero);
        public new static Result<T> Fail(string message) => Fail(null, message, TimeSpan.Zero);

        public static implicit operator Result<T>((T Value, TimeSpan Elapsed) t) => Ok(t.Value, t.Elapsed);
        public static implicit operator Result<T>(T value) => Ok(value);

        public static implicit operator Result<T>((Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result<T>((Exception Exception, string Message) t) => Fail(t.Exception, t.Message);
        public static implicit operator Result<T>((Exception Exception, TimeSpan Elapsed) t) => Fail(t.Exception, t.Elapsed);
        public static implicit operator Result<T>(Exception exception) => Fail(exception);

        public static implicit operator T(Result<T> result) => result.Value;
    }
}
