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

    public class Result<TValue> : Result
    {
        private readonly TValue _value;

        protected Result(Exception exception, string message, TimeSpan elapsed) : base(exception, message, elapsed) { }
        protected Result(TValue value, TimeSpan elapsed) : this(null, null, elapsed) => _value = value;

        public TValue Value => Succees ? _value : throw new InvalidOperationException("Value isn't available because the result is in error state.");

        public IEnumerable<TValue> AsEnumerable<TValue>() => Value as IEnumerable<TValue> ?? throw new InvalidCastException($"Cannot cast {typeof(TValue).Name} to {typeof(TValue).Name}.");

        public static Result<TValue> Ok(TValue value, TimeSpan elapsed) => new Result<TValue>(value, elapsed);
        public static Result<TValue> Ok(TValue value) => Ok(value, TimeSpan.Zero);

        public new static Result<TValue> Fail(Exception exception, string message, TimeSpan elapsed) => new Result<TValue>(exception, message, elapsed);
        public new static Result<TValue> Fail(Exception exception, string message) => new Result<TValue>(exception, message, TimeSpan.Zero);
        public new static Result<TValue> Fail(Exception exception, TimeSpan elapsed) => Fail(exception, null, elapsed);
        public new static Result<TValue> Fail(Exception exception) => Fail(exception, null, TimeSpan.Zero);
        public new static Result<TValue> Fail(string message) => Fail(null, message, TimeSpan.Zero);

        public static implicit operator Result<TValue>((TValue Value, TimeSpan Elapsed) t) => Ok(t.Value, t.Elapsed);
        public static implicit operator Result<TValue>(TValue value) => Ok(value);

        public static implicit operator Result<TValue>((Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result<TValue>((Exception Exception, string Message) t) => Fail(t.Exception, t.Message);
        public static implicit operator Result<TValue>((Exception Exception, TimeSpan Elapsed) t) => Fail(t.Exception, t.Elapsed);
        public static implicit operator Result<TValue>(Exception exception) => Fail(exception);

        public static implicit operator TValue(Result<TValue> result) => result.Value;
    }

    public class Result<TTarget, TValue> : Result
    {
        private readonly TTarget _target;
        private readonly TValue _value;

        protected Result(TTarget target, Exception exception, string message, TimeSpan elapsed) : base(exception, message, elapsed) { _target = target; }
        protected Result(TTarget target, TValue value, TimeSpan elapsed) : this(target, null, null, elapsed) => _value = value;

        public TTarget Target => _target;
        public TValue Value => Succees ? _value : throw new InvalidOperationException("Value isn't available because the result is in error state.");

        public IEnumerable<TElement> AsEnumerable<TElement>() => Value as IEnumerable<TElement> ?? throw new InvalidCastException($"Cannot cast {typeof(TValue).Name} to {typeof(TElement).Name}.");

        public static Result<TTarget, TValue> Ok(TTarget target, TValue value, TimeSpan elapsed) => new Result<TTarget, TValue>(target, value, elapsed);
        public static Result<TTarget, TValue> Ok(TTarget target, TValue value) => Ok(target, value, TimeSpan.Zero);

        public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, string message, TimeSpan elapsed) => new Result<TTarget, TValue>(target, exception, message, elapsed);
        public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, string message) => new Result<TTarget, TValue>(target, exception, message, TimeSpan.Zero);
        public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, TimeSpan elapsed) => Fail(target, exception, null, elapsed);
        public static Result<TTarget, TValue> Fail(TTarget target, Exception exception) => Fail(target, exception, null, TimeSpan.Zero);
        public static Result<TTarget, TValue> Fail(TTarget target, string message) => Fail(target, null, message, TimeSpan.Zero);

        public static implicit operator Result<TTarget, TValue>((TTarget Target, TValue Value, TimeSpan Elapsed) t) => Ok(t.Target, t.Value, t.Elapsed);
        public static implicit operator Result<TTarget, TValue>((TTarget Target, TValue Value) t) => Ok(t.Target, t.Value);

        public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Target, t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, string Message) t) => Fail(t.Target, t.Exception, t.Message);
        public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, TimeSpan Elapsed) t) => Fail(t.Target, t.Exception, t.Elapsed);
        public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception) t) => Fail(t.Target, t.Exception);

        public static implicit operator TValue(Result<TTarget, TValue> result) => result.Value;
    }
}
