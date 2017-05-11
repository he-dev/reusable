using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Reusable.Extensions;

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

        protected Result(Exception exception, string message, IEnumerable<Result> innerResults)
        {
            Exception = exception;
            Message = message;
            InnerResults = innerResults.ToImmutableList();
            Elapsed = InnerResults.Aggregate(TimeSpan.Zero, (current, next) => current.Add(next.Elapsed));
        }

        public Exception Exception { get; }
        public string Message { get; }
        public TimeSpan Elapsed { get; }
        public IImmutableList<Result> InnerResults { get; }

        public bool Succees => string.IsNullOrEmpty(Message) && Exception == null;
        public bool Failure => !Succees;

        public override string ToString() => Succees ? Message : (Exception?.Message ?? Message);

        public static Result Ok(TimeSpan elapsed) => new Result(null, null, elapsed);
        public static Result Ok() => Ok(TimeSpan.Zero);

        public static Result Fail(Exception exception, string message, TimeSpan elapsed) => new Result(exception, message, elapsed);
        public static Result Fail(Exception exception, string message, IEnumerable<Result> innerResults) => new Result(exception, message, innerResults);

        public static Result Fail(Exception exception, string message) => Fail(exception, message, TimeSpan.Zero);
        public static Result Fail(Exception exception, TimeSpan elapsed) => Fail(exception, null, elapsed);
        public static Result Fail(Exception exception, IEnumerable<Result> innerResults) => Fail(exception, null, innerResults);
        public static Result Fail(Exception exception) => Fail(exception, null, TimeSpan.Zero);
        public static Result Fail(string message, TimeSpan elapsed) => Fail(null, message, elapsed);
        public static Result Fail(string message, IEnumerable<Result> innerResults) => Fail(null, message, innerResults);
        public static Result Fail(string message) => Fail(null, message, TimeSpan.Zero);

        public static Result All(params Func<Result>[] actions)
        {
            foreach (var action in actions)
            {
                var result = action();
                if (!result) return result;
            }
            return Ok();
        }

        public static implicit operator Result((Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result((Exception Exception, string Message, IEnumerable<Result> InnerResults) t) => Fail(t.Exception, t.Message, t.InnerResults);
        public static implicit operator Result((Exception Exception, string Message) t) => Fail(t.Exception, t.Message);
        public static implicit operator Result((Exception Exception, TimeSpan Elapsed) t) => Fail(t.Exception, t.Elapsed);
        public static implicit operator Result((Exception Exception, IEnumerable<Result> InnerResults) t) => Fail(t.Exception, t.InnerResults);
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

    public class Result<TValue> : Result, IDisposable
    {
        private readonly TValue _value;

        protected Result(Exception exception, string message, TimeSpan elapsed) : base(exception, message, elapsed) { }
        protected Result(Exception exception, string message, IEnumerable<Result> innerResults) : base(exception, message, innerResults) { }
        protected Result(TValue value, TimeSpan elapsed) : this(null, null, elapsed) => _value = value;

        public TValue Value => Succees ? _value : throw new InvalidOperationException("Value isn't available because the result is in error state.");

        public IEnumerable<TElement> AsEnumerable<TElement>() => Value as IEnumerable<TElement> ?? throw new InvalidCastException($"Cannot cast {typeof(TValue).Name} to {typeof(TElement).Name}.");
        public IEnumerable<TElement> AsCollection<TElement>() => Value as ICollection<TElement> ?? throw new InvalidCastException($"Cannot cast {typeof(TValue).Name} to {typeof(TElement).Name}.");

        public static Result<TValue> Ok(TValue value, TimeSpan elapsed) => new Result<TValue>(value, elapsed);
        public static Result<TValue> Ok(TValue value, IEnumerable<Result> innerResults) => Ok(value, innerResults.Aggregate(TimeSpan.Zero, (current, next) => current.Add(next.Elapsed)));
        public static Result<TValue> Ok(TValue value) => Ok(value, TimeSpan.Zero);

        public new static Result<TValue> Fail(Exception exception, string message, TimeSpan elapsed) => new Result<TValue>(exception, message, elapsed);
        public new static Result<TValue> Fail(Exception exception, string message, IEnumerable<Result> innerResults) => new Result<TValue>(exception, message, innerResults);
        public new static Result<TValue> Fail(Exception exception, string message) => new Result<TValue>(exception, message, TimeSpan.Zero);
        public new static Result<TValue> Fail(Exception exception, TimeSpan elapsed) => Fail(exception, null, elapsed);
        public new static Result<TValue> Fail(Exception exception, IEnumerable<Result> innerResults) => Fail(exception, null, innerResults);
        public new static Result<TValue> Fail(Exception exception) => Fail(exception, null, TimeSpan.Zero);
        public new static Result<TValue> Fail(string message, TimeSpan elapsed) => Fail(null, message, elapsed);
        public new static Result<TValue> Fail(string message, IEnumerable<Result> innerResults) => Fail(null, message, innerResults);
        public new static Result<TValue> Fail(string message) => Fail(null, message, TimeSpan.Zero);
        public new static Result<TValue> Fail(Result result, string message) => Fail(result.Exception, message, result.Elapsed);

        public static Result<TValue> Conditional(Func<bool> predicate, Func<TValue> onSuccess, Func<string> onFailure)
        {
            return
                predicate()
                    ? Ok(onSuccess())
                    : Fail(onFailure());
        }

        public static implicit operator Result<TValue>((TValue Value, TimeSpan Elapsed) t) => Ok(t.Value, t.Elapsed);
        public static implicit operator Result<TValue>(TValue value) => Ok(value);

        public static implicit operator Result<TValue>((Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Exception, t.Message, t.Elapsed);
        public static implicit operator Result<TValue>((Exception Exception, string Message, IEnumerable<Result> InnerResults) t) => Fail(t.Exception, t.Message, t.InnerResults);
        public static implicit operator Result<TValue>((Exception Exception, string Message) t) => Fail(t.Exception, t.Message);
        public static implicit operator Result<TValue>((Exception Exception, TimeSpan Elapsed) t) => Fail(t.Exception, t.Elapsed);
        public static implicit operator Result<TValue>((Exception Exception, IEnumerable<Result> InnerResults) t) => Fail(t.Exception, t.InnerResults);
        public static implicit operator Result<TValue>(Exception exception) => Fail(exception);

        public static implicit operator TValue(Result<TValue> result) => result.Value;

        public void Dispose()
        {
            if (Succees && Value is IDisposable disposable) disposable.Dispose();
        }
    }

    public static class ResultExtensions
    {
        //public static Result OnSuccess(this Result @this, Func<Result> @do) => @this.Succees ? @do() : @this;
        //public static Result OnFailure(this Result @this, Func<Result> @do) => @this.Failure ? @do() : @this;

        //public static Result OnSuccess<T>(this Result<T> result, Func<T, Result> @do) => result.Succees ? @do(result.Value) : result;
        ////public static Result<T> OnFailure<T>(this Result<T> @this, Func<Result<T>> @do) => @this.Failure ? @do() : @this;

        //public static Result<T> Do<T>(this Result<T> result, Action<T> action) => result.Tee(r => action(r.Value));

        //public static Result Then<T>(this Result<T> result, Action<T> action)
        //{
        //    using (result)
        //    {
        //        return result.Succees ? Try.Execute(() => action(result.Value)) : result;
        //    }
        //}

        //public static bool Then<T>(this Result<T> result, out Result actionResult, Action<T> action)
        //{
        //    using (result)
        //    {
        //        if (result.Succees)
        //        {
        //            return Try.Execute(out actionResult, () => action(result.Value));
        //        }
        //        else
        //        {
        //            actionResult = result;
        //            return false;
        //        }
        //    }
        //}

        //public void Dispose()
        //{
        //    if (Succees && Value is IDisposable disposable) disposable.Dispose();
        //}
    }

    // This wasn't a good idea but...
    //public class Result<TTarget, TValue> : Result
    //{
    //    private readonly TTarget _target;
    //    private readonly TValue _value;

    //    protected Result(TTarget target, Exception exception, string message, TimeSpan elapsed) : base(exception, message, elapsed) { _target = target; }
    //    protected Result(TTarget target, TValue value, TimeSpan elapsed) : this(target, null, null, elapsed) => _value = value;

    //    public TTarget Target => _target;
    //    public TValue Value => Succees ? _value : throw new InvalidOperationException("Value isn't available because the result is in error state.");

    //    public IEnumerable<TElement> AsEnumerable<TElement>() => Value as IEnumerable<TElement> ?? throw new InvalidCastException($"Cannot cast {typeof(TValue).Name} to {typeof(TElement).Name}.");

    //    public static Result<TTarget, TValue> Ok(TTarget target, TValue value, TimeSpan elapsed) => new Result<TTarget, TValue>(target, value, elapsed);
    //    public static Result<TTarget, TValue> Ok(TTarget target, TValue value) => Ok(target, value, TimeSpan.Zero);

    //    public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, string message, TimeSpan elapsed) => new Result<TTarget, TValue>(target, exception, message, elapsed);
    //    public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, string message) => new Result<TTarget, TValue>(target, exception, message, TimeSpan.Zero);
    //    public static Result<TTarget, TValue> Fail(TTarget target, Exception exception, TimeSpan elapsed) => Fail(target, exception, null, elapsed);
    //    public static Result<TTarget, TValue> Fail(TTarget target, Exception exception) => Fail(target, exception, null, TimeSpan.Zero);
    //    public static Result<TTarget, TValue> Fail(TTarget target, string message, TimeSpan elapsed) => Fail(target, null, message, elapsed);
    //    public static Result<TTarget, TValue> Fail(TTarget target, string message) => Fail(target, null, message, TimeSpan.Zero);

    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, TValue Value, TimeSpan Elapsed) t) => Ok(t.Target, t.Value, t.Elapsed);
    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, TValue Value) t) => Ok(t.Target, t.Value);

    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, string Message, TimeSpan Elapsed) t) => Fail(t.Target, t.Exception, t.Message, t.Elapsed);
    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, string Message) t) => Fail(t.Target, t.Exception, t.Message);
    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception, TimeSpan Elapsed) t) => Fail(t.Target, t.Exception, t.Elapsed);
    //    public static implicit operator Result<TTarget, TValue>((TTarget Target, Exception Exception) t) => Fail(t.Target, t.Exception);

    //    public static implicit operator TValue(Result<TTarget, TValue> result) => result.Value;
    //}
}
