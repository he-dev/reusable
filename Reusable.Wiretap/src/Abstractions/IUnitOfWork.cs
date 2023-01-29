using System.Runtime.CompilerServices;

namespace Reusable.Wiretap.Abstractions;

public interface IUnitOfWork
{
    UnitOfWork.Context Begin
    (
        string name,
        object? details = default,
        object? attachment = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    );
}